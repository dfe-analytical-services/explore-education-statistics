import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export default function middleware(req: NextRequest) {
  if (process.env.BASIC_AUTH) {
    switch (process.env.NEXT_PUBLIC_URL) {
      case 'https://dev.explore-education-statistics.service.gov.uk':
      case 'https://test.explore-education-statistics.service.gov.uk':
      case 'https://preprod.explore-education-statistics.service.gov.uk': {
        const basicAuth = req.headers.get('Authorization')?.split(' ')[1];
        const url = req.nextUrl;

        if (basicAuth) {
          const [username, password] = atob(basicAuth).split(':');

          if (
            username === process.env.BASIC_AUTH_USER &&
            password === process.env.BASIC_AUTH_PASSWORD
          ) {
            return NextResponse.next();
          }
        }

        url.pathname = '/api/basicauth';
        return NextResponse.rewrite(url);
      }

      default:
        return NextResponse.next();
    }
  }

  return NextResponse.next();
}
