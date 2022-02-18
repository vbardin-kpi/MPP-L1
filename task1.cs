internal static class Program
{
    private const int TopN = 25;
    private const int StopWordsCount = 2;
    private const string FilePath = @"C:\Users\vbardin\Desktop\Task 1.txt";

    private static readonly string[] StopWords = {"the", "a"};
    
    public static unsafe void Main()
    {
        var textToProcess = File.ReadAllText(FilePath);
        var textToProcessLength = 0;
        var whitespaces = 0;

        countTextLength:
        {
            try
            {
                if (textToProcess[textToProcessLength] != '\0')
                {
                    textToProcessLength++;
                    goto countTextLength;
                }
            }
            catch
            {
                textToProcessLength--;
            }
        }

        // Convert text to lower case
        if (textToProcessLength == 0)
        {
            return;
        }

        fixed (char* pSource = textToProcess)
        {
            var charIdx = 0;
            // Convert letter to lower case and count whitespaces
            toLowerCycleStart:
            {
                uint letter = *(pSource + charIdx);

                if (letter - 'A' <= 'Z' - 'A')
                {
                    letter += 32;
                    *(pSource + charIdx) = (char) letter;
                }

                if (letter is ' ' or ',' or '!' or '?' or '.')
                {
                    whitespaces++;
                }
            }

            charIdx++;

            if (charIdx < textToProcessLength)
            {
                goto toLowerCycleStart;
            }
        }

        // Split words
        var indexer = 0;
        var words = new string[whitespaces + 1];

        var savedAmount = 0;

        var firstWhiteSpaceIdx = 0;
        var wordStartIndex = indexer;
        var wordEndIndex = indexer;

        findWhiteSpace:
        if (textToProcess[indexer] == ' ')
        {
            firstWhiteSpaceIdx = indexer;
            // to exit from cycle
            indexer = textToProcessLength;
        }

        indexer++;
        if (indexer < textToProcessLength)
        {
            goto findWhiteSpace;
        }

        wordEndIndex = firstWhiteSpaceIdx == wordStartIndex
            ? textToProcessLength
            : firstWhiteSpaceIdx;

        words[savedAmount] = textToProcess[wordStartIndex..wordEndIndex];
        savedAmount++;

        // cause the word starts after the whitespace character
        var currPos = firstWhiteSpaceIdx + 1;
        wordStartIndex = currPos;
        wordEndIndex = currPos;

        findWordEnd:
        if (textToProcess[currPos] == ' ' ||
            textToProcess[currPos] == ',' ||
            textToProcess[currPos] == '!' ||
            textToProcess[currPos] == '?' ||
            textToProcess[currPos] == '.')
        {
            wordEndIndex = currPos;
            words[savedAmount] = textToProcess[wordStartIndex..wordEndIndex];
            savedAmount++;

            wordStartIndex = wordEndIndex += 1;
        }

        currPos++;
        if (currPos < textToProcessLength)
        {
            goto findWordEnd;
        }

        wordEndIndex = wordStartIndex == wordEndIndex
            ? textToProcessLength
            : wordEndIndex;

        words[savedAmount] = textToProcess[wordStartIndex..wordEndIndex];

        // Remove stop words
        var stopWords = 0;

        var stwIdx = 0;
        removeStopWords:
        {
            var currStopWord = StopWords[stwIdx];

            var currTtpWordIdx = 0;
            internalCycle:
            {
                if (words[currTtpWordIdx] == currStopWord)
                {
                    words[currTtpWordIdx] = "";
                    stopWords++;
                }

                currTtpWordIdx++;
            }
            if (currTtpWordIdx < whitespaces + 1)
            {
                goto internalCycle;
            }

            currTtpWordIdx = 0;
            stwIdx++;
        }

        if (stwIdx < StopWordsCount)
        {
            goto removeStopWords;
        }

        var tfDescriptors = new TermFrequencyDescriptor[whitespaces + 1 - stopWords];
        var descriptorsExists = 0;

        var currWordIdx = 0;
        createTermFrequencyDescriptors:
        {
            var descriptorFound = false;

            if (words[currWordIdx] != "")
            {
                var tfdIdx = 0;
                findTfxDescriptor:
                {
                    if (tfdIdx < descriptorsExists &&
                        tfDescriptors[tfdIdx].Term == words[currWordIdx])
                    {
                        tfDescriptors[tfdIdx].Frequency++;
                        descriptorFound = true;
                    }

                    tfdIdx++;
                }
                if (tfdIdx < descriptorsExists)
                {
                    goto findTfxDescriptor;
                }

                if (!descriptorFound)
                {
                    var tfd = new TermFrequencyDescriptor(words[currWordIdx], 1);
                    tfDescriptors[descriptorsExists] = tfd;
                    descriptorsExists++;
                }
            }

            currWordIdx++;
        }
        if (currWordIdx < whitespaces + 1)
        {
            goto createTermFrequencyDescriptors;
        }

        // Create terms descriptors and order them
        var currTfdIdx = 0;
        order:
        {
            var idx = 0;
            innerLoop:
            {
                if (tfDescriptors[idx].Frequency < tfDescriptors[idx + 1].Frequency)
                {
                    (tfDescriptors[idx], tfDescriptors[idx + 1]) =
                        (tfDescriptors[idx + 1], tfDescriptors[idx]);
                }

                idx++;
            }
            if (idx < whitespaces - stopWords - currTfdIdx)
            {
                goto innerLoop;
            }

            currTfdIdx++;
        }
        if (currTfdIdx < whitespaces - stopWords)
        {
            goto order;
        }

        // take top N

        var mostlyUsed = new TermFrequencyDescriptor[TopN];
        var descriptorsSelected = 0;
        currTfdIdx = 0;
        takeTopN:
        {
            if (tfDescriptors[currTfdIdx].Term != "")
            {
                mostlyUsed[descriptorsSelected] = tfDescriptors[currTfdIdx];
                descriptorsSelected++;
            }
            else
            {
                goto printRes;
            }

            currTfdIdx++;
        }
        if (currTfdIdx < descriptorsExists &&
            currTfdIdx < TopN)
        {
            goto takeTopN;
        }

        printRes:
        {
            var pIdx = 0;
            printCycle:
            {
                Console.WriteLine(mostlyUsed[pIdx]);
                pIdx++;
            }
            if (pIdx < descriptorsSelected)
            {
                goto printCycle;
            }
        }
    }
}

struct TermFrequencyDescriptor
{
    public string Term { get; set; }
    public int Frequency { get; set; }

    public TermFrequencyDescriptor(string term, int frequency)
    {
        Term = term;
        Frequency = frequency;
    }

    public override string ToString()
    {
        return $"{Term} - {Frequency}";
    }
}