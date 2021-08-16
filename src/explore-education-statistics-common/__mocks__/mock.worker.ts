export default class MockWorker {
  onmessage: () => void;

  constructor() {
    this.onmessage = () => {};
  }

  postMessage() {
    this.onmessage();
  }
}
