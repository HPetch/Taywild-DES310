using System.Collections.Generic;

namespace DialogueSystem.Data.Error
{
    using Elements;
    public class DialogueSystemNodeErrorData
    {
        public DialogueSystemErrorData ErrorData { get; set; }
        public List<DialogueSystemNode> Nodes { get; set; }

        public DialogueSystemNodeErrorData()
        {
            ErrorData = new DialogueSystemErrorData();
            Nodes = new List<DialogueSystemNode>();
        }
    }
}