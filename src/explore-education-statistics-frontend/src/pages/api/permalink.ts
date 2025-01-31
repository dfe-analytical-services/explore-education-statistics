import createPermalinkTable from '@frontend/modules/api/permalink/createPermalinkTable';
import withMethods from '@frontend/middleware/api/withMethods';

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
