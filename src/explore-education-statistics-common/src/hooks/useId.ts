import { useId as useAutoId } from '@reach/auto-id';

export interface UseIdOptions {
  /**
   * Overrides the generated id with a
   * custom id. No prefix is applied.
   */
  id?: string;
  prefix: string;
}

/**
 * Generate an id for HTML element with that is unique and stable for SSR.
 */
export default function useId({ id, prefix }: UseIdOptions): string {
  const autoId = useAutoId();

  return id || `${prefix}-${autoId}`;
}
