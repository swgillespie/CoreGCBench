__BuildConfig=Debug
__Restore=1

while :; do
    if [ $# -le 0 ]; then
        break
    fi

    case $1 in
        -\?|-h|--help)
            echo "usage: build.sh [debug] [release] [skiprestore]"
            exit 1
            ;;
        debug)
            __BuildConfig=Debug
            ;;
        release)
            __BuildConfig=release
            ;;
        skiprestore)
            __Restore=0
            ;;
        *)
            ;;
    esac

    shift
done

if [ $__Restore == 1 ]; then
    dotnet restore CoreGCBench.Core.sln
fi

dotnet build CoreGCBench.Core.sln --configuration $__BuildConfig
dotnet publish CoreGCBench.Core.sln --configuration $__BuildConfig