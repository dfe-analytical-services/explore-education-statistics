import { Dictionary } from '@common/types';

const boundaryTypesMap: Dictionary<{
  label: string;
  description: string;
}> = {
  BFC: {
    label: 'BFC',
    description:
      'Full resolution - clipped to the coastline (Mean High Water mark)',
  },
  BFE: {
    label: 'BFE',
    description:
      'Full resolution - extent of the realm (usually this is the Mean Low Water mark but, in some cases, boundaries extend beyond this to include offshore islands)',
  },
  BGC: {
    label: 'BGC',
    description:
      'Generalised (20m) - clipped to the coastline (Mean High Water mark)',
  },
  BUC: {
    label: 'BUC',
    description:
      'Ultra Generalised (500m) - clipped to the coastline (Mean High Water mark)',
  },
};

export default boundaryTypesMap;
