public interface IMicSocket
{
    float angle { get; }
    int vad { get; }
    bool isConnected { get; }
    string classification { get; }
    float distanceProxy {get; }
    float realDistance {get; }
    
    
}