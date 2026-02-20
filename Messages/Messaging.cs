using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShoppingList002.Messages
{
    public class VoiceSearch_VM_to_VoiceSearchPage_CategoryCreatedMessage : ValueChangedMessage<int>
    {
        public VoiceSearch_VM_to_VoiceSearchPage_CategoryCreatedMessage(int categoryId) : base(categoryId) { }
    }
    public class VoiceSearch_VM_to_CandidateCategoryPage_CategoryCreatedMessage : ValueChangedMessage<int>
    {
        public VoiceSearch_VM_to_CandidateCategoryPage_CategoryCreatedMessage(int categoryId) : base(categoryId) { }
    }
    public class VoiceSearch_VM_to_VoiceAddPage_CategoryCreatedMessage : ValueChangedMessage<int>
    {
        public VoiceSearch_VM_to_VoiceAddPage_CategoryCreatedMessage(int categoryId) : base(categoryId) { }
    }
}
