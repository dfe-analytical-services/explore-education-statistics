import withMethods from '@frontend/middleware/api/withMethods';
import searchTableTool from '@frontend/modules/api/table-tool-search/searchTableTool';

export default withMethods({
  post: searchTableTool,
});
