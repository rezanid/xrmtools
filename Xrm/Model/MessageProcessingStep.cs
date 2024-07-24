namespace XrmGen.Xrm.Model;

public class MessageProcessingStep
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool AsyncAutoDelete { get; set; }
    public object CustomConfiguration { get; set; }
    public object Description { get; set; }
    public string FilteringAttributes { get; set; }
    public string ImpersonatingUserFullname { get; set; }
    public int InvocationSource { get; set; }
    public string MessageName { get; set; }
    public int Mode { get; set; }
    public string PrimaryEntityName { get; set; }
    public int Rank { get; set; }
    public string SdkMessageId { get; set; }
    public int Stage { get; set; }
    public int StateCode { get; set; }
    public int SupportedDeployment { get; set; }
    public MessageProcessingStepImage[] Images { get; set; }
}