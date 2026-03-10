using System.Data;
using System.Text.RegularExpressions;
using ProfanityService.DataAccess.Interfaces;
using ProfanityService.Service.Interfaces;

namespace ProfanityService.Service;

public class ProfanityService(IProfanityRepository profanityRepository): IProfanityService
{

    public async Task<string> FilterProfanityAsync(string text, char replacementChar = '*')
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new NoNullAllowedException();
        }

        var profanities = await profanityRepository.GetProfaneWords();
        var filteredText = text;
    
        foreach (var profanity in profanities)
        {
            var profanityText = profanity.ToString();
        
            if (string.IsNullOrWhiteSpace(profanityText))
            {
                continue;
            }
        
            var pattern = $@"\b{Regex.Escape(profanityText)}\b";
            var replacement = new string(replacementChar, profanityText.Length);
        
            filteredText = Regex.Replace(filteredText, pattern, replacement, RegexOptions.IgnoreCase);
        }
    
        return filteredText;
    }
    
    public async Task<bool> ContainsProfanityAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var profanities = await profanityRepository.GetProfaneWords();
    
        foreach (var profanity in profanities)
        {
            var profanityText = profanity.ToString();
        
            if (string.IsNullOrWhiteSpace(profanityText))
            {
                continue;
            }
        
            var pattern = $@"\b{Regex.Escape(profanityText)}\b";
        
            if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }
    
        return false;
    }

}