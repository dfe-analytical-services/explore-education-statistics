import React, { FunctionComponent, ReactNode } from 'react';
import FormCheckboxGroup, {
  CheckboxGroupChangeEventHandler,
} from '../../../components/FormCheckboxGroup';
import styles from './FilterMenu.module.scss';
import MenuDetails from './MenuDetails';

interface Props {
  beforeMenu?: ReactNode;
  onChange: CheckboxGroupChangeEventHandler;
}

const FilterMenu: FunctionComponent<Props> = ({ beforeMenu, onChange }) => {
  return (
    <div className={styles.filterMenu}>
      {beforeMenu}

      <div>
        <MenuDetails summary="Schools (under 16)" open>
          <MenuDetails summary="Absence and exclusions" open>
            <FormCheckboxGroup
              name="absenceAndExclusions"
              onChange={onChange}
              options={[
                {
                  checked: true,
                  id: 'pupilAbsence',
                  label: 'Pupil absence',
                  value: 'PUPIL_ABSENCE',
                },
                {
                  checked: true,
                  id: 'exclusions',
                  label: 'Exclusions',
                  value: 'EXCLUSIONS',
                },
              ]}
            />
          </MenuDetails>
          <MenuDetails summary="Capacity and admissions" />
          <MenuDetails summary="Results" />
          <MenuDetails summary="School and pupil numbers" />
          <MenuDetails summary="School finance" />
          <MenuDetails summary="Teacher numbers" />
        </MenuDetails>
        <MenuDetails summary="16+ education">
          <MenuDetails summary="Absence and exclusions" />
          <MenuDetails summary="Capacity and admissions" />
          <MenuDetails summary="Results" />
          <MenuDetails summary="School and pupil numbers" />
          <MenuDetails summary="School finance" />
          <MenuDetails summary="Teacher numbers" />
        </MenuDetails>
        <MenuDetails summary="Social care">
          <MenuDetails summary="Absence and exclusions" />
          <MenuDetails summary="Capacity and admissions" />
          <MenuDetails summary="Results" />
          <MenuDetails summary="School and pupil numbers" />
          <MenuDetails summary="School finance" />
          <MenuDetails summary="Teacher numbers" />
        </MenuDetails>
      </div>
    </div>
  );
};

export default FilterMenu;
