import { RefinedResponse } from 'k6/http';

export function isRefinedResponse(
  candidate: unknown,
): candidate is RefinedResponse<'text'> {
  return !!(
    candidate &&
    typeof candidate === 'object' &&
    'error' in candidate &&
    'error_code' in candidate
  );
}

export function pickRandom<T>(arr: T[]) {
  return arr[Math.floor(Math.random() * arr.length)];
}

export function pickRandomItems<T>(arr: T[], numberOfItems: number): T[] {
  return arr
    .sort(() => 0.5 - Math.random())
    .slice(0, Math.ceil(numberOfItems * arr.length));
}

export function stringifyWithoutNulls(obj: object) {
  return JSON.stringify(obj, (_, value) => (!value ? undefined : value), 2);
}

export function parseIntOptional(int: string) {
  return parseInt(int, 10) || undefined;
}
