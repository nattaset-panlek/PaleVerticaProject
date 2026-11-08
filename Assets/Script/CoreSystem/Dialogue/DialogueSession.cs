namespace CoreSystem.Dialogue
{
    public sealed class DialogueSession
    {
        #region [Variables/Fields]
        private readonly DialogueModel _model;
        private DialogueNode _current;
        #endregion

        #region [Properties]
        public DialogueNode Current => _current;
        public bool IsFinished => _current == null;
        #endregion

        #region [Public Methods]
        public DialogueSession(DialogueModel model)
        {
            _model = model;
            _current = model != null ? Find(model.StartNode) : null;
        }

        public void Choose(int choiceIndex)
        {
            if (_current == null || _current.Choices == null) return;
            if (choiceIndex < 0 || choiceIndex >= _current.Choices.Count) return;
            _current = Find(_current.Choices[choiceIndex].Next);
        }

        public void End()
        {
            _current = null;
        }
        #endregion

        #region [Private Methods]
        private DialogueNode Find(int id)
        {
            if (_model == null || _model.Nodes == null) return null;
            return _model.Nodes.Find(node => node.Id == id);
        }
        #endregion
    }
}
