import React, { forwardRef } from 'react'

interface TextAreaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string
  error?: string
  helperText?: string
  fullWidth?: boolean
}

const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(
  ({ label, error, helperText, fullWidth = false, className = '', ...props }, ref) => {
    const textAreaClasses = `
      w-full px-3 py-2 rounded-lg border bg-surface text-text-primary
      focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-secondary/60 focus-visible:ring-offset-2 focus-visible:ring-offset-brand-bg
      disabled:bg-brand-bg disabled:text-text-muted disabled:cursor-not-allowed
      ${error ? 'border-state-error' : 'border-border-muted'}
      ${className}
    `

    return (
      <div className={fullWidth ? 'w-full' : ''}>
        {label && (
          <label className="block text-sm font-medium text-text-secondary mb-1">
            {label}
            {props.required && <span className="text-state-error ml-1">*</span>}
          </label>
        )}
        <textarea ref={ref} className={textAreaClasses} {...props} />
        {error && <p className="mt-1 text-sm text-state-error">{error}</p>}
        {helperText && !error && <p className="mt-1 text-sm text-text-muted">{helperText}</p>}
      </div>
    )
  }
)

TextArea.displayName = 'TextArea'

export default TextArea
