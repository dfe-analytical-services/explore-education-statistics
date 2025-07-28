// import LoadingSpinner from '@common/components/LoadingSpinner';
// import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { RouteComponentProps } from 'react-router';
// import { useQuery } from '@tanstack/react-query';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';

// export const MethodologyContentPageInternal = () => {
//   const {
//     methodology,
//     methodologyVersion,
//     canUpdateMethodology,
//     isPreRelease,
//   } = useMethodologyContentState();

//   const canUpdateContent = !isPreRelease && canUpdateMethodology;

//   return (
//     <EditingContextProvider editingMode={canUpdateContent ? 'edit' : 'preview'}>
//       {canUpdateContent && <EditablePageModeToggle />}

//       <div className="govuk-width-container">
//         {isPreRelease ? (
//           <PageTitle caption="Methodology" title={methodology.title} />
//         ) : (
//           <h2 aria-hidden className="govuk-heading-lg" data-testid="page-title">
//             {methodology.title}
//           </h2>
//         )}

//         <MethodologyContent
//           methodology={methodology}
//           methodologyVersion={methodologyVersion}
//         />
//       </div>
//     </EditingContextProvider>
//   );
// };

const EducationInNumbersContentPage = ({
  match,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const { educationInNumbersPageId } = match.params;

  // const { data: pageContent, isLoading } = useQuery(
  //   educationInNumbersPageContentQueries.get(educationInNumbersPageId),
  // );

  return (
    // <LoadingSpinner loading={isLoading}>
    //   {pageContent ? (
    //     <p>Page content</p>
    //   ) : (
    //     <WarningMessage>Could not load page content</WarningMessage>
    //   )}
    // </LoadingSpinner>
    <p>Page content - {educationInNumbersPageId}</p>
  );
};

export default EducationInNumbersContentPage;
