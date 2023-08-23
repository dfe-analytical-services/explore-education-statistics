export class CancellablePromise<T> extends Promise<T> {
  cancel(): void {}
}
