import * as Yup from 'yup';

export default class FileSchema extends Yup.MixedSchema<File> {
  constructor() {
    super();

    this.required = this.required.bind(this);

    this.withMutation(() => {
      // eslint-disable-next-line func-names, @typescript-eslint/no-explicit-any
      this.transform(function (value: any, originalValue: any) {
        if (originalValue === null || originalValue === undefined) {
          return this.notRequired();
        }

        return value;
      });
    });
  }

  required(message?: string) {
    return this.test({
      name: 'required',
      message: message || 'Required',
      exclusive: true,
      test(value) {
        return !!value;
      },
    });
  }

  minSize(minBytes: number, message?: string) {
    return this.test({
      name: 'minSize',
      message: message || 'File must be larger than 0 bytes',
      exclusive: true,
      test(value) {
        if (!value) {
          return true;
        }

        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-ignore
        return value.size > minBytes;
      },
    });
  }
}
