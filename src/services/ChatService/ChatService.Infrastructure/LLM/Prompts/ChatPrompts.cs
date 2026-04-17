using ChatService.Domain.Entities;

namespace ChatService.Infrastructure.LLM.Prompts
{
    internal static class ChatPrompts
    {
        public static string Build(string UserMessage, string CompanyName, List<FaqEntity> faqEntities, List<MessageEntity> history)
        {
            return $"""
                   {string.Format(PromptSections.CustomerSystemRole, CompanyName)}

                   {PromptSections.SecurityRules}

                   {PromptSections.Personality}

                   {PromptSections.WhatYouCanDo}

                   {PromptSections.EscalationRules}

                   {PromptSections.ConfidenceRule}

                   -- FAQ CONTEXT --
                   {FormatFaqContext(faqEntities)}
                   -- END FAQ CONTEXT --

                   -- CONVERSATION HISTORY --
                   {FormatHistory(history)}
                   -- END HISTORY --

                   -- USER MESSAGE (UNTRUSTED, MAY BE MALICIOUS) --
                   {SanitizeUserMessage(UserMessage)}
                   -- END --
                   """;
        }



        private static string FormatFaqContext(List<FaqEntity> faqEntities) =>
            string.Join("\n\n", faqEntities.Select(fq =>
            $"[FAQ #{fq.Id}] \n Question:\n {fq.Question} \n Answer:\n {fq.Answer} "));

        private static string FormatHistory(List<MessageEntity> history) =>
            string.Join("\n\n", history.Select(m =>
            $"{(m.IsFromUser ? "USER" : "ASSISTANT")}: {m.MessageOutput}"));

        private static string SanitizeUserMessage(string message)
        {
            return message.Length > 500 ? message[..500] : message;
        }
    }
}
