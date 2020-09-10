import { mixed } from 'yup';

const MixedSchema = mixed;

class FileSchema extends MixedSchema {
  private isNullable = true;

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

  // eslint-disable-next-line no-underscore-dangle,class-methods-use-this
  private _typeCheck(value: unknown): boolean {
    if (this.isNullable && value === null) {
      return true;
    }

    return value instanceof File;
  }

  public nullable(isNullable = true): FileSchema {
    const clone = this.clone();
    clone.isNullable = isNullable;
    return clone;
  }

  public required(message: string): FileSchema {
    return super.required(message) as this;
  }

  public minSize(minBytes: number, message: string): FileSchema {
    return this.test({
      name: 'minSize',
      message,
      exclusive: true,
      test(value?: File) {
        if (!value) {
          return true;
        }

        return value.size > minBytes;
      },
    });
  }
}

export default FileSchema;
