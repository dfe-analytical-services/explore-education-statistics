export default function checkStringIsGuidShape(
  str: string | undefined | null,
): boolean {
  const guidRegex = /^[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}$/;
  return !str ? false : guidRegex.test(str);
}
