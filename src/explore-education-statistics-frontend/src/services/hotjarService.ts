import { hotjar } from 'react-hotjar';

// eslint-disable-next-line import/prefer-default-export
export const initHotJar = () => {
  if (
    process.env.HOTJAR_ID !== undefined &&
    process.env.HOTJAR_TRACKING === 'true'
  ) {
    hotjar.initialize(process.env.HOTJAR_ID, 6);

    // eslint-disable-next-line no-console
    console.log('Hotjar initialised');
  }
};
