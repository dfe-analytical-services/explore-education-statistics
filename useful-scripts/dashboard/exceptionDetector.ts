// Heuristics for spotting exceptions across the different log formats this
// project's services produce: .NET's default console logger (dotnet run),
// the Azure Functions host (func-based services), and Next.js/Node (frontend).
// Intentionally matches only "header" lines (e.g. `System.Exception: ...`),
// not stack trace continuation lines (`   at Foo.Bar()`), so one exception
// produces one alert instead of one per frame.
const EXCEPTION_PATTERNS: RegExp[] = [
  /^\s*fail:\s/, // .NET default console logger, Error level
  /\[(ERR|FTL)\]/, // Serilog default console theme
  /unhandled exception/i,
  /\bException\b.*:/, // e.g. "System.NullReferenceException: ..."
  /Executed '.*'\s*\(Failed/, // Azure Functions host reporting a failed invocation
  /^Error:\s/, // Node uncaught error headers
  /^UnhandledPromiseRejectionWarning/i,
  /^⨯/, // Next.js dev server error marker
];

export default function looksLikeException(line: string): boolean {
  return EXCEPTION_PATTERNS.some(pattern => pattern.test(line));
}
