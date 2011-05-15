/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter
{
    public enum FilterType
    {
        None,
        Text,
        RegexText,
        Name,
        RegexName,
        Source,
        RegexSource,
        ExText,
        ExTextRegex,
        ExName,
        ExNameRegex,
        ExSource,
        ExSourceRegex,
    }
}
