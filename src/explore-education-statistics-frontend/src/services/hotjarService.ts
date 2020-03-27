import hotjarTracker from './utils/hotjarTracker';

// eslint-disable-next-line import/prefer-default-export
export const initHotJar = () => {
  if (process.env.HOTJAR_ID) {
    hotjarTracker(process.env.HOTJAR_ID, 6);

    // eslint-disable-next-line no-console
    console.log('Hotjar initialised');
  }
};
