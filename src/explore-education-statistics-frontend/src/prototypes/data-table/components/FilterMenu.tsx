import React, { FunctionComponent } from 'react';
import Button from '../../../components/Button';
import FormCheckboxGroup, {
  CheckboxGroupChangeEventHandler,
} from '../../../components/FormCheckboxGroup';
import styles from './FilterMenu.module.scss';
import MenuDetails from './MenuDetails';

interface Props {
  onChange: CheckboxGroupChangeEventHandler;
}

const FilterMenu: FunctionComponent<Props> = ({ onChange }) => {
  return (
    <div className={styles.filterMenu}>
      <h3 className="govuk-heading-s">Choose publications or indicators</h3>

      <form>
        <div className="govuk-form-group">
          <input type="text" className="govuk-input" />
        </div>

        <Button>Search</Button>
      </form>

      <MenuDetails summary="School statistics (under 16)" open>
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
    </div>
  );
};

export default FilterMenu;
