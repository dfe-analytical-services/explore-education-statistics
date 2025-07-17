/**
 * Custom text wrapping: creates an array of lines of words
 * from a single string to fit a character limit per line
 */
export default function chunkLabelToFitCharLimit(
  label: string,
  maxLineChars: number = 20,
): string[] {
  return label
    .split(' ') // split into words
    .reduce(
      (wordArrays: string[][], word) => {
        const lastArray = wordArrays.at(-1);
        // Get the length of the last array (joined)
        const currLen = lastArray!.join(' ').length;

        // If the length of that content and the new word
        // exceeds maxLineChars push the word to a new array
        if (currLen + 1 + word.length > maxLineChars) {
          wordArrays.push([word]);
        } else {
          // otherwise add it to the existing array
          lastArray!.push(word);
        }

        return wordArrays;
      },
      [[]],
    )
    .map(words => words.join(' ')); // Join up arrays into lines of words
}
