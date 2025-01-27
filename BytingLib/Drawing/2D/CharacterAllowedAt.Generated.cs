namespace BytingLib
{
    public static class CharacterAllowedAt
	{
        public static bool BeginningOfLine(char c)
        {
            return c switch
            {
               '.' or ',' or ';' or '!' or '?' or ':' or ']' or ')' or '}' or '>' or '€' or '$' or '%' or '¢' or '°' or '·' or '†' or '‡' or '›' or '℃' or '∶' or '、' or '。' or '〃' or '〆' or '〕' or '〗' or '〞' or '﹚' or '﹜' or '！' or '＂' or '％' or '＇' or '）' or '，' or '．' or '：' or '；' or '？' or '］' or '｝' or '～' or '〉' or '》' or '」' or '』' or '】' or '〙' or '〟' or '\'' or '｠' or '»' or 'ヽ' or 'ヾ' or 'ー' or 'ァ' or 'ィ' or 'ゥ' or 'ェ' or 'ォ' or 'ッ' or 'ャ' or 'ュ' or 'ョ' or 'ヮ' or 'ヵ' or 'ヶ' or 'ぁ' or 'ぃ' or 'ぅ' or 'ぇ' or 'ぉ' or 'っ' or 'ゃ' or 'ゅ' or 'ょ' or 'ゎ' or 'ゕ' or 'ゖ' or 'ㇰ' or 'ㇱ' or 'ㇲ' or 'ㇳ' or 'ㇴ' or 'ㇵ' or 'ㇶ' or 'ㇷ' or 'ㇸ' or 'ㇹ' or 'ㇺ' or 'ㇻ' or 'ㇼ' or 'ㇽ' or 'ㇾ' or 'ㇿ' or '々' or '〻' or '‐' or '゠' or '–' or '〜' or '‼' or '⁇' or '⁈' or '⁉' or '・' => false,
                _ => true
            };
        }

        public static bool EndOfLine(char c)
        {
            return c switch
            {
                '(' or '[' or '{' or '$' or '£' or '¥' or '·' or '〈' or '《' or '「' or '『' or '【' or '〔' or '〖' or '〝' or '﹙' or '﹛' or '＄' or '（' or '［' or '｛' or '￡' or '￥' or '〘' or '\'' or '｟' or '«' => false,
                _ => true
            };
        }
	}
}