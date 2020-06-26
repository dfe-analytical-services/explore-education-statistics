type StorageSetterOptions = {
  expiry?: Date;
};

type StorageItem<T = unknown> = {
  expiry?: string;
  value: T;
};

const storageService = {
  get<T>(key: string): Promise<T | null> {
    return Promise.resolve().then(() => {
      const item = localStorage.getItem(key);

      if (!item) {
        return null;
      }

      const { value, expiry } = JSON.parse(item) as StorageItem<T>;

      if (expiry && new Date() >= new Date(expiry)) {
        this.remove(key);
        return null;
      }

      return value;
    });
  },
  set(
    key: string,
    value: unknown,
    options: StorageSetterOptions = {},
  ): Promise<void> {
    return Promise.resolve().then(() => {
      const { expiry } = options;

      let item: StorageItem = { value };

      if (expiry) {
        // Don't bother setting in storage
        // as it would already be expired.
        if (expiry <= new Date()) {
          return;
        }

        item = { value, expiry: expiry.toISOString() };
      }

      localStorage.setItem(key, JSON.stringify(item));
    });
  },
  remove(key: string): Promise<void> {
    return Promise.resolve().then(() => localStorage.removeItem(key));
  },
  clear(): Promise<void> {
    return Promise.resolve().then(() => localStorage.clear());
  },
};

export default storageService;
