import Link from '@admin/components/Link';
import boundaryDataService from '@admin/services/boundaryDataService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useCallback, useMemo } from 'react';
import FormProvider from '@common/components/form/FormProvider';
import FormGroup from '@common/components/form/FormGroup';
import FormFieldset from '@common/components/form/FormFieldset';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Page from '@admin/components/Page';
import { useQuery } from '@tanstack/react-query';
import boundaryDataQueries from '@admin/queries/boundaryDataQueries';
import { useHistory, useParams } from 'react-router-dom';
import Yup from '@common/validation/yup';
import { ObjectSchema } from 'yup';

export default function BoundaryLevelEditPage() {
  interface FormValues {
    boundaryLevelLabel: string;
  }

  const { id } = useParams<{ id: string }>();
  const history = useHistory();
  const boundaryDataPath = '/administration/boundary-data';

  const {
    data: boundaryLevel = null,
    isLoading: isLoadingBoundaryLevel,
    refetch: reloadBoundaryLevel,
  } = useQuery(boundaryDataQueries.getBoundaryLevel(id));

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      await boundaryDataService.updateBoundaryLevel({
        id,
        label: values.boundaryLevelLabel,
      });

      reloadBoundaryLevel();
      history.push(boundaryDataPath);
    },
    [id, boundaryDataPath, reloadBoundaryLevel, history],
  );

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      boundaryLevelLabel: Yup.string().required('Enter a boundary level name'),
    });
  }, []);

  return (
    <Page
      title="Boundary data"
      caption="Edit boundary level"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Boundary data', link: '/administration/boundary-data' },
        { name: 'Boundary level' },
      ]}
    >
      <LoadingSpinner loading={isLoadingBoundaryLevel}>
        {!id ? (
          <>
            <p>Boundary level not found.</p>
            <Link unvisited to={boundaryDataPath}>
              Go back
            </Link>
          </>
        ) : (
          <FormProvider enableReinitialize validationSchema={validationSchema}>
            <Form id="updateBoundaryLevelForm" onSubmit={handleSubmit}>
              <FormFieldset
                id="boundaryData"
                legend="Update boundary level label"
                legendSize="l"
                hint="This is the text shown on the drop down list when selecting a chart boundary."
              >
                <FormGroup>
                  <FormFieldTextInput<FormValues>
                    id="label"
                    name="boundaryLevelLabel"
                    label="Label"
                    className="govuk-!-width-one-half"
                    defaultValue={boundaryLevel?.label}
                  />
                </FormGroup>

                <ButtonGroup>
                  <Link unvisited to={boundaryDataPath}>
                    Cancel
                  </Link>
                  <Button type="submit">Save boundary level</Button>
                </ButtonGroup>
              </FormFieldset>
            </Form>
          </FormProvider>
        )}
      </LoadingSpinner>
    </Page>
  );
}
