import createPermalinkTable from '@frontend/modules/api/permalink/createPermalinkTable';
import withMethods from '@frontend/middleware/withMethods';

export default withMethods({
  post: createPermalinkTable,
});
