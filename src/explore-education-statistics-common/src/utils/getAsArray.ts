export default function getAsArray(
  val: string | string[] | undefined,
): string[] | undefined {
  if (!val) return undefined;
  return Array.isArray(val) ? val : [val];
}
