/**
 * The message from a thrown value, whatever it turned out to be.
 *
 * `catch` gives back `unknown`, and plenty of what reaches these handlers
 * isn't an `Error` at all - execa rejections, string throws from libraries -
 * so `err.message` can't be relied on.
 */
export default function errorMessage(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
}
