import { NextMiddlewareResult } from 'next/dist/server/web/types';
import {
  NextFetchEvent,
  NextProxy,
  NextRequest,
  NextResponse,
} from 'next/server';

export type ChainedMiddleware = (
  request: NextRequest,
  event: NextFetchEvent,
  next: NextProxy,
) => NextMiddlewareResult | Promise<NextMiddlewareResult>;

export default function chain(middlewares: ChainedMiddleware[]): NextProxy {
  const [current, ...remaining] = middlewares;

  if (current) {
    const next = chain(remaining);
    return (request, event) => current(request, event, next);
  }

  return () => NextResponse.next();
}
