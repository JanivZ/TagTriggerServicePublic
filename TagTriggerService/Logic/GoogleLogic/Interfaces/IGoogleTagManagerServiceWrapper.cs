using Google.Apis.TagManager.v2;

namespace TagTriggerService.Logic.GoogleLogic
{
    public interface IGoogleTagManagerServiceWrapper
    {
        string AccountAndContainerPath { get; }
        string AccountPath { get; }
        string ContainerPath { get; }
        TagManagerService Service { get; }
    }
}