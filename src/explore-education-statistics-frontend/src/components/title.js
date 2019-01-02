import React, { Component } from 'react';
import PropTypes from 'prop-types'

class Title extends Component {
    render() {
        return (
            <div class="app-content__header">
                <span class="govuk-caption-xl">{this.props.label}</span>
                <h1 class="govuk-heading-xl">Find {(this.props.label || '').toLowerCase()} statistics</h1>
            </div>
        );
    }
}

Title.propTypes = {
    label: PropTypes.string
}

Title.defaultProps = {
    label: ''
}

export default Title;