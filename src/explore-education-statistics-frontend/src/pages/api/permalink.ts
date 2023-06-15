import withMethods from '@frontend/middleware/api/withMethods';
import createPermalinkTable from '@frontend/modules/api/permalink/createPermalinkTable';

export const config = {
  api: {
    bodyParser: {
      sizeLimit: '75mb',
    },
  },
};

export default withMethods({
  post: createPermalinkTable,
});
