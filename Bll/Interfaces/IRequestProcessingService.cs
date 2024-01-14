namespace Bll.Interfaces;

public interface IRequestProcessingService
{
    bool StartProcessing(string key);
    void FinishProcessing(string key);
}