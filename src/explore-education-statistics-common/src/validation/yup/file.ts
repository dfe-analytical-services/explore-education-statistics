import * as Yup from 'yup';

export default class FileSchema extends Yup.MixedSchema<File> {
  constructor() {
    super({ type: 'file' });

    this.withMutation(() => {
      this.transform(value => {
        if (this.isType(value)) {
          return value;
        }

        return null;
      });
    });
  }

  required(message: string): FileSchema {
    return super.required(message) as this;
  }

  minSize(minBytes: number, message?: string): FileSchema {
    return this.test({
      name: 'minSize',
      message: message ?? 'File must be larger than 0 bytes',
      exclusive: true,

      test(value) {
        if (!value) {
          return true;
        }

        return value.size > minBytes;
      },
    });
  }

  maxSize(maxBytes: number, message?: string): FileSchema {
    return this.test({
      name: 'maxSize',
      message: message ?? `File must be smaller than ${maxBytes} bytes`,
      exclusive: true,

      test(value) {
        if (!value) {
          return true;
        }
        return value.size < maxBytes;
      },
    });
  }
}
