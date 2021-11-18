import { NextRouter } from 'next/router';

const mockRouter: NextRouter = {
  isFallback: false,
  isLocaleDomain: false,
  isPreview: false,
  isReady: true,
  basePath: '',
  pathname: '/',
  route: '/',
  asPath: '/',
  query: {},
  push: jest.fn(),
  replace: jest.fn(),
  reload: jest.fn(),
  back: jest.fn(),
  prefetch: jest.fn(),
  beforePopState: jest.fn(),
  events: {
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
  },
};

export function useRouter() {
  return {
    route: '/',
    pathname: '',
    query: '',
    asPath: '',
  };
}

export default mockRouter;
