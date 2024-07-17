import { Dictionary } from '@common/types';

const boundaryTypesMap: Dictionary<{
  label: string;
  description: string;
}> = {
  BFE: {
    label: 'BFE',
    description:
      'Full Extent - full resolution boundaries go to the Extent of the Realm (Low Water Mark) and are the most detailed of the boundaries',
  },
  BFC: {
    label: 'BFC',
    description:
      'Full Clipped - full resolution boundaries that are clipped to the coastline (Mean High Water mark)',
  },
  BGC: {
    label: 'BGC',
    description:
      'Generalised Clipped - generalised to 20m and clipped to the coastline (Mean High Water mark) and more generalised than the BFE boundaries',
  },
  BSC: {
    label: 'BSC',
    description:
      'Super Generalised Clipped - generalised to 200m and clipped to the coastline (Mean High Water mark)',
  },
  BUC: {
    label: 'BUC',
    description:
      'Ultra Generalised Clipped - generalised to 500m and clipped to the coastline (Mean High Water mark)',
  },

  BGE: {
    label: 'BGE',
    description:
      'Grid, Extent - grid formed of equally sized cells which extend beyond the coastline',
  },
  BGG: {
    label: 'BGG',
    description: 'Generalised, Grid - generalised 50m grid squares',
  },
};

export default boundaryTypesMap;
