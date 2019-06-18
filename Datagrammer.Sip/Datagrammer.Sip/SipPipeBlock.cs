using Datagrammer.Middleware;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Sip
{
    public sealed class SipPipeBlock : MiddlewareBlock<Datagram, Datagram>, ISourceBlock<SipResponse>, ISourceBlock<SipRequest>
    {
        private readonly IPropagatorBlock<SipRequest, SipRequest> requestBuffer;
        private readonly IPropagatorBlock<SipResponse, SipResponse> responseBuffer;

        public SipPipeBlock() : this(new SipPipeOptions())
        {
        }

        public SipPipeBlock(SipPipeOptions options) : base(options?.MiddlewareOptions)
        {
            responseBuffer = new BufferBlock<SipResponse>(new DataflowBlockOptions
            {
                BoundedCapacity = options.ResponseBufferCapacity
            });

            requestBuffer = new BufferBlock<SipRequest>(new DataflowBlockOptions
            {
                BoundedCapacity = options.RequestBufferCapacity
            });
        }

        protected async override Task ProcessAsync(Datagram value)
        {
            await NextAsync(value);

            if (SipParser.TryParseResponse(value.Buffer, out var response))
            {
                await responseBuffer.SendAsync(response);
            }
            else if (SipParser.TryParseRequest(value.Buffer, out var request))
            {
                await requestBuffer.SendAsync(request);
            }
        }

        protected override Task AwaitCompletionAsync()
        {
            return Task.WhenAll(responseBuffer.Completion, requestBuffer.Completion);
        }

        protected override void OnComplete()
        {
            responseBuffer.Complete();
            requestBuffer.Complete();
        }

        protected override void OnFault(Exception exception)
        {
            responseBuffer.Fault(exception);
            requestBuffer.Fault(exception);
        }

        public SipRequest ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<SipRequest> target, out bool messageConsumed)
        {
            return requestBuffer.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public SipResponse ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<SipResponse> target, out bool messageConsumed)
        {
            return responseBuffer.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<SipRequest> target, DataflowLinkOptions linkOptions)
        {
            return requestBuffer.LinkTo(target, linkOptions);
        }

        public IDisposable LinkTo(ITargetBlock<SipResponse> target, DataflowLinkOptions linkOptions)
        {
            return responseBuffer.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<SipRequest> target)
        {
            requestBuffer.ReleaseReservation(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<SipResponse> target)
        {
            responseBuffer.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<SipRequest> target)
        {
            return requestBuffer.ReserveMessage(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<SipResponse> target)
        {
            return responseBuffer.ReserveMessage(messageHeader, target);
        }
    }
}
