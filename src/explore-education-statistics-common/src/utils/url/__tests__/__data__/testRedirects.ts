import { Redirects } from '@common/services/redirectService';

const testRedirects: Redirects = {
  methodologies: [
    {
      fromSlug: 'test-methodology',
      toSlug: 'test-methodology-revised',
    },
  ],
  publications: [
    {
      fromSlug: 'test-publication',
      toSlug: 'test-publication-revised',
    },
  ],
};

export default testRedirects;
