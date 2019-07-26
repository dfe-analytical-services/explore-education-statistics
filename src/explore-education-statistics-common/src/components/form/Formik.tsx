import {
  Formik as BaseFormik,
  FormikTouched,
  FormikValues,
  setNestedObjectValues,
} from 'formik';

/**
 * Component extending Formik to workaround various
 * limitations with the current v1 API.
 *
 * It would be preferable if Formik would provide these
 * parts for us instead (maybe in the upcoming v2 release...)
 */
class Formik<FormValues = FormikValues> extends BaseFormik<FormValues> {
  /**
   * Hack to get around Formik's `submitForm` method not
   * chaining promises if `onSubmit` handler also returns a promise.
   *
   * The following issues describes this better, but
   * has not been addressed since March 2018!!!
   * @see https://github.com/jaredpalmer/formik/issues/486
   */
  public submitForm = () => {
    // Recursively set all values to `true`.
    this.setState(prevState => ({
      touched: setNestedObjectValues<FormikTouched<FormValues>>(
        prevState.values,
        true,
      ),
      isSubmitting: true,
      isValidating: true,
      submitCount: prevState.submitCount + 1,
    }));

    this.setState({
      values: this.preSubmit(),
    });

    return this.runValidations(this.state.values).then(async combinedErrors => {
      if (this.didMount) {
        this.setState({ isValidating: false });
      }
      const isValid = Object.keys(combinedErrors).length === 0;

      try {
        if (isValid) {
          await this.executeSubmit();
        }
      } finally {
        if (this.didMount) {
          // ^^^ Make sure Formik is still mounted before calling setState
          this.setState({ isSubmitting: false });
        }
      }
    });
  };

  public preSubmit = () => {
    if (this.props.preSubmit) {
      return this.props.preSubmit(this.state.values);
    }
    return this.state.values;
  };

  public executeSubmit = () => {
    return this.props.onSubmit(this.state.values, this.getFormikActions());
  };
}

export default Formik;
