'use client'

import { Component, ErrorInfo, ReactNode } from 'react'
import { Card, Button } from '@/components/common'
import { AlertTriangle, RotateCcw } from 'lucide-react'

interface Props {
  children: ReactNode
  fallback?: ReactNode
}

interface State {
  hasError: boolean
  error?: Error
}

export default class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo)
  }

  handleReset = () => {
    this.setState({ hasError: false, error: undefined })
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback
      }

      return (
        <div className="min-h-screen flex items-center justify-center p-4 bg-brand-bg">
          <Card padding="lg" className="max-w-md w-full text-center">
            <div className="flex justify-center mb-4">
              <div className="bg-state-error/15 p-4 rounded-full">
                <AlertTriangle className="w-10 h-10 text-state-error" />
              </div>
            </div>
            <h2 className="text-2xl font-bold text-text-primary mb-2">
              Oops! Something went wrong
            </h2>
            <p className="text-text-secondary mb-6">
              We're sorry for the inconvenience. The application encountered an unexpected error.
            </p>
            {this.state.error && process.env.NODE_ENV === 'development' && (
              <div className="mb-6 p-4 bg-brand-bg rounded-lg text-left">
                <p className="text-xs font-mono text-state-error break-all">
                  {this.state.error.message}
                </p>
              </div>
            )}
            <div className="flex gap-3 justify-center">
              <Button variant="primary" onClick={this.handleReset}>
                <RotateCcw className="mr-2 w-4 h-4" />
                Try Again
              </Button>
              <Button variant="secondary" onClick={() => window.location.href = '/'}>
                Go Home
              </Button>
            </div>
          </Card>
        </div>
      )
    }

    return this.props.children
  }
}
