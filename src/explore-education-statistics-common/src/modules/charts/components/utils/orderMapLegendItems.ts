import naturalOrderBy from '@common/utils/array/naturalOrderBy';
import { LegendConfiguration, LegendItem } from '../../types/legend';

export default function orderMapLegendItems(
  legend: LegendConfiguration,
): LegendItem[] {
  return naturalOrderBy(legend.items ?? [], 'label');
}
