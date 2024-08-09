interface Props<T> {
  get: () => Promise<T>;
  initial: T;
  duration?: number;
}

interface CachedValue {
  value: object;
  fetchedAt: number;
}

interface CacheInterface<T> {
  get: () => Promise<T>;
}

let cachedT: CachedValue;

function createMemoryCache<T>({
  get,
  initial,
  duration,
}: Props<T>): CacheInterface<T> {
  cachedT = cachedT ?? {
    value: initial,
    fetchedAt: Date.now(),
  };

  const getCachedT: () => Promise<T> = async (): Promise<T> => {
    const cacheTime = duration ?? getCacheTime();
    const shouldRefetch =
      !cachedT || cachedT.fetchedAt + cacheTime < Date.now();

    if (shouldRefetch) {
      cachedT = {
        value: (await get()) as object,
        fetchedAt: Date.now(),
      };
    }

    return cachedT.value as T;
  };

  return { get: getCachedT };
}

function getCacheTime(): number {
  switch (process.env.APP_ENV) {
    case 'Local':
      return 2_000;
    case 'Development':
      return 10_000;
    default:
      return 60_000;
  }
}

export default createMemoryCache;
