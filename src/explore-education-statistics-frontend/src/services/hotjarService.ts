import hotjarTracker from './utils/hotjarTracker';

// eslint-disable-next-line import/prefer-default-export
export const initHotJar = (trackingId: string) => {
  if (trackingId) {
    hotjarTracker(trackingId, 6);

    // eslint-disable-next-line no-console
    console.log('Hotjar initialised');
  }
};
