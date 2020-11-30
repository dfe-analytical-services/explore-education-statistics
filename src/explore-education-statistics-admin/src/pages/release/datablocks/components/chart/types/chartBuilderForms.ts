import { FormState } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';

export interface ChartBuilderForm extends FormState {
  title: string;
  id: string;
}

export interface ChartBuilderForms {
  options: ChartBuilderForm;
  data: ChartBuilderForm;
  legend: ChartBuilderForm;
  [key: string]: ChartBuilderForm;
}
