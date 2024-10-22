namespace ChatCommands
{
    public abstract class BaseExecutionMethod
    {
        protected string id;

        public string Id
            => id;
        public virtual int MaxResponseLength
            => -1;

        public abstract void SendResponse(object executorDetails, BaseCommandResponse response);

        public abstract bool HasPermission(object executorDetails, string permission);
    }

    public class ChatExecutionMethod : BaseExecutionMethod
    {
        public ChatExecutionMethod()
            => id = "chat";

        public override int MaxResponseLength
            => Utility.MaxChatMessageLength;

        public override void SendResponse(object executorDetails, BaseCommandResponse response)
        {
            if (response is StyledCommandResponse styledResponse)
            {
                string title = null;
                if (response.CommandResponseType == CommandResponseType.Public)
                    foreach (string line in response.GetFormattedResponse())
                    {
                        if (title == null)
                        {
                            title = line;
                            continue;
                        }
                        Utility.SendMessage(line, Utility.MessageType.Styled, title);
                        if (styledResponse.OnlyTitleFirstLine && title.Length != 0)
                            title = string.Empty;
                    }
                else
                    foreach (string line in response.GetFormattedResponse())
                    {
                        if (title == null)
                        {
                            title = line;
                            continue;
                        }
                        Utility.SendMessage((ulong)executorDetails, line, Utility.MessageType.Styled, title);
                        if (styledResponse.OnlyTitleFirstLine && title.Length != 0)
                            title = string.Empty;
                    }
                return;
            }

            if (response.CommandResponseType == CommandResponseType.Public)
                foreach (string line in response.GetFormattedResponse())
                    Utility.SendMessage(line);
            else
                foreach (string line in response.GetFormattedResponse())
                    Utility.SendMessage((ulong)executorDetails, line);
        }

        public override bool HasPermission(object clientDetails, string permission)
            => PermissionGroupsCompatibility.Enabled
                ? PermissionGroupsCompatibility.PermissionGroupHasPermission(PermissionGroupsCompatibility.GetClientPermissionGroup((ulong)clientDetails), permission)
                : (ulong)clientDetails == Utility.HostClientId;
    }
}