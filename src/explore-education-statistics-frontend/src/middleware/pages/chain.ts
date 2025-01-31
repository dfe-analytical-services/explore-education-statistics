import { NextMiddlewareResult } from 'next/dist/server/web/types';
import {
  NextFetchEvent,
  NextMiddleware,
  NextRequest,
  NextResponse,
} from 'next/server';

export type ChainedMiddleware = (
  request: NextRequest,
  event: NextFetchEvent,
  next: NextMiddleware,
) => NextMiddlewareResult | Promise<NextMiddlewareResult>;

export default function chain(
  middlewares: ChainedMiddleware[],
): NextMiddleware {
  const [current, ...remaining] = middlewares;

  if (current) {
    const next = chain(remaining);
    return (request, event) => current(request, event, next);
  }

  return () => NextResponse.next();
}
