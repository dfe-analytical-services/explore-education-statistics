import boundaryDataService from '@admin/services/boundaryDataService';
import React, { useCallback, useMemo } from 'react';
import Page from '@admin/components/Page';
import FormProvider from '@common/components/form/FormProvider';
import FormFieldset from '@common/components/form/FormFieldset';
import FormGroup from '@common/components/form/FormGroup';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import Button from '@common/components/Button';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import {
  Form,
  FormFieldSelect,
  FormFieldTextInput,
} from '@common/components/form';
import { useHistory } from 'react-router-dom';
import ButtonGroup from '@common/components/ButtonGroup';
import Link from '@admin/components/Link';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import Yup from '@common/validation/yup';
import { SelectOption } from '@common/components/form/FormSelect';
import { ObjectSchema } from 'yup';

export default function BoundaryDataUploadPage() {
  interface FormValues {
    boundaryLevel: string;
    boundaryLevelLabel: string;
    boundaryLevelPublishedDate: Date;
    boundaryDataFile: File;
  }

  const history = useHistory();
  const boundaryDataPath = '/administration/boundary-data';

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      const data = {
        level: values.boundaryLevel,
        label: values.boundaryLevelLabel,
        file: values.boundaryDataFile,
        published: values.boundaryLevelPublishedDate,
      };

      await boundaryDataService.uploadBoundaryFile(data);
      history.push(boundaryDataPath);
    },
    [boundaryDataPath, history],
  );

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      boundaryLevel: Yup.string().required('Select a boundary level type'),
      boundaryLevelLabel: Yup.string().required('Enter a boundary level name'),
      boundaryDataFile: Yup.file()
        .required('Select a boundary file')
        .maxSize(134217728, 'Boundary file must be under 128mb in size'),
      boundaryLevelPublishedDate: Yup.date().required(
        'Enter a boundary file publication date',
      ),
    });
  }, []);

  return (
    <Page
      title="Boundary data"
      caption="Upload boundary data"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Boundary data', link: '/administration/boundary-data' },
        { name: 'Upload boundary data' },
      ]}
    >
      <FormProvider enableReinitialize validationSchema={validationSchema}>
        {({ formState }) => {
          const options: SelectOption<string | number>[] = [
            { value: '', label: '' },
          ];

          Object.keys(locationLevelsMap).map(key => {
            return options.push({
              value: locationLevelsMap[key].label.replace(/\s/g, ''),
              label: `${locationLevelsMap[key].code} - ${locationLevelsMap[key].label}`,
            });
          });

          return (
            <Form id="addBoundaryDataForm" onSubmit={handleSubmit}>
              <FormFieldset
                id="boundaryData"
                legend="Upload new boundary data"
                legendSize="l"
                hint="This file will be processed and added to the list of available boundary files on publications."
              >
                <FormGroup>
                  <FormFieldSelect<FormValues>
                    id="boundaryLevel"
                    name="boundaryLevel"
                    label="Select boundary level"
                    className="govuk-!-width-one-third"
                    options={options}
                  />

                  <FormFieldTextInput<FormValues>
                    id="boundaryLevelLabel"
                    name="boundaryLevelLabel"
                    label="Label"
                    hint="Boundary level label must be unique"
                    className="govuk-!-width-one-third"
                  />

                  <FormFieldDateInput<FormValues>
                    id="boundaryLevelPublishedDate"
                    name="boundaryLevelPublishedDate"
                    hint="The date the file was released to the public"
                    legend="Published"
                    legendSize="s"
                  />
                </FormGroup>

                <FormFieldFileInput<FormValues>
                  id="boundaryDataFile"
                  name="boundaryDataFile"
                  label="Upload new boundary data file"
                  hint="Must be a GeoJSON file type, and no larger than 128mb"
                />
              </FormFieldset>

              <ButtonGroup>
                <Link unvisited to={boundaryDataPath}>
                  Cancel
                </Link>
                <Button disabled={formState.isSubmitting} type="submit">
                  Upload file
                </Button>
              </ButtonGroup>
            </Form>
          );
        }}
      </FormProvider>
    </Page>
  );
}
