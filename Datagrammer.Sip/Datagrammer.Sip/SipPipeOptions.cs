using Datagrammer.Middleware;

namespace Datagrammer.Sip
{
    public sealed class SipPipeOptions
    {
        public int ResponseBufferCapacity { get; set; } = 1;

        public int RequestBufferCapacity { get; set; } = 1;

        public MiddlewareOptions MiddlewareOptions { get; set; } = new MiddlewareOptions();
    }
}
